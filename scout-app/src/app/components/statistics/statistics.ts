import { Component, AfterViewInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EventService } from '../../services/event';
import { Chart, registerables } from 'chart.js';
import { PermissionService } from '../../services/permission';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { AuthService } from '../../services/auth';

Chart.register(...registerables);

@Component({
  selector: 'app-statistics',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './statistics.html',
  styleUrl: './statistics.css',
})
export class Statistics implements AfterViewInit {
  totalEvents = 0;
  totalAttendees = 0;
  avgAttendees = 0;
  totalRevenue = 0;
  isAdmin: boolean = false;
  isLoading = false;

  chart1: any;
  chart2: any;
  chart3: any;

  constructor(
    private service: EventService,
    private authService: AuthService,
    private permissionService: PermissionService,
    private cdr: ChangeDetectorRef
  ) {
    this.isLoading = true;
  }

  // first load html charts, then run this code
  ngAfterViewInit() {
    this.service.getAll('all', '', 'all', -1, -1).subscribe(events => {
      this.isLoading = false;
      this.renderChart1(events);
      this.renderChart2(events);
      this.totalEvents = events.length;
      this.totalAttendees = events.reduce((sum, e) => sum + (e.attendees ? e.attendees.length : 0), 0);
      this.avgAttendees = this.totalEvents > 0 ? Number((this.totalAttendees / this.totalEvents).toFixed(1)) : 0;
      this.totalRevenue = events.reduce((sum, e) => sum + (e.price * (e.attendees ? e.attendees.length : 0)), 0);
      this.cdr.detectChanges();
    });

    this.authService.getAllUsers().subscribe(users => {
      this.renderChart3(users);
    });

    this.isAdmin = this.permissionService.isAdmin();
  }

  private renderChart1(events: any[]) {
    this.chart1 = new Chart('chart1', {
      type: 'bar',
      data: {
        labels: events.map(e => e.name),
        datasets: [{
          label: 'Attendees',
          data: events.map(e => e.attendees ? e.attendees.length : 0),
          backgroundColor: '#a3bfa0',
          barPercentage: 0.6
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12 } }
        },
        scales: {
          y: { beginAtZero: true, suggestedMax: Math.max(...events.map(e => e.attendees ? e.attendees.length : 0)) + 1 },
          x: { ticks: { maxRotation: 45, minRotation: 45, font: { size: 10 } } }
        }
      }
    });
  }

  private renderChart2(events: any[]) {
    let free = 0;
    let upTo25 = 0;
    let between25and50 = 0;
    let over50 = 0;

    events.forEach(e => {
      if (e.price === 0) free++;
      else if (e.price <= 25) upTo25++;
      else if (e.price <= 50) between25and50++;
      else over50++;
    });

    const total = events.length;
    const formatLabel = (count: number) => total > 0 ? `${Math.round((count / total) * 100)}%` : '0%';

    this.chart2 = new Chart('chart2', {
      type: 'bar',
      data: {
        labels: [`Free: ${formatLabel(free)}`, `$1-$25: ${formatLabel(upTo25)}`, `$26-$50: ${formatLabel(between25and50)}`, `$51+: ${formatLabel(over50)}`],
        datasets: [{
          label: 'Events',
          data: [free, upTo25, between25and50, over50],
          backgroundColor: ['#8ba37c', '#b5c1a4', '#7d5a39', '#9e7b4f'],
          barPercentage: 0.6
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12 } }
        },
        scales: {
          y: { beginAtZero: true },
          x: { ticks: { maxRotation: 45, minRotation: 45, font: { size: 10 } } }
        }
      }
    });
  }

  private renderChart3(users: any[]) {
    let lupisori = 0;
    let temerari = 0;
    let exploratori = 0;
    let seniori = 0;
    let lideri = 0;

    users.forEach(u => {
      if (u.scoutLevel === 'Lupisor') lupisori++;
      else if (u.scoutLevel === 'Temerar') temerari++;
      else if (u.scoutLevel === 'Explorator') exploratori++;
      else if (u.scoutLevel === 'Senior') seniori++;
      else if (u.scoutLevel === 'Lider') lideri++;
    });

    let maxCount = Math.max(lupisori, temerari, exploratori, seniori, lideri);

    this.chart3 = new Chart('chart3', {
      type: 'bar',
      data: {
        labels: ['Lupisori', 'Temerari', 'Exploratori', 'Seniori', 'Lideri'],
        datasets: [{
          label: 'Scouts',
          data: [lupisori, temerari, exploratori, seniori, lideri],
          backgroundColor: ['#7c9a6f' , '#a3bfa0', '#5c4033', '#9e7b4f', '#c49a6f'],
          barPercentage: 0.7
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { position: 'bottom', labels: { boxWidth: 12 } }
        },
        scales: {
          y: { beginAtZero: true, suggestedMax: maxCount + 1 },
          x: { ticks: { maxRotation: 45, minRotation: 45, font: { size: 10 } } }
        }
      }
    });
  }

  toggleChart(num: number) {
    const chart = num === 1 ? this.chart1 : num === 2 ? this.chart2 : this.chart3;
    if (!chart) return;

    const isBar = chart.config.type === 'bar';
    chart.config.type = isBar ? 'pie' : 'bar';
    
    if (chart.options && chart.options.scales) {
      if (chart.options.scales['x']) {
        chart.options.scales['x'].display = !isBar;
      }
      if (chart.options.scales['y']) {
        chart.options.scales['y'].display = !isBar;
      }
    }
    
    chart.update();
  }
}
