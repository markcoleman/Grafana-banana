import { Component, signal, OnInit, inject, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { WeatherService, WeatherForecast } from './weather.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private weatherService = inject(WeatherService);
  
  protected readonly title = signal('Grafana-banana');
  protected weatherData = signal<WeatherForecast[]>([]);
  protected loading = signal(false);
  protected error = signal<string | null>(null);
  protected isDarkMode = signal(false);

  constructor() {
    // Initialize dark mode from system preference or localStorage
    if (typeof window !== 'undefined') {
      const savedMode = localStorage.getItem('darkMode');
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.isDarkMode.set(savedMode === 'true' || (!savedMode && prefersDark));
      
      // Apply dark mode effect
      effect(() => {
        if (this.isDarkMode()) {
          document.documentElement.classList.add('dark');
        } else {
          document.documentElement.classList.remove('dark');
        }
      });
    }
  }

  ngOnInit() {
    this.loadWeatherData();
  }

  loadWeatherData() {
    this.loading.set(true);
    this.error.set(null);
    
    this.weatherService.getWeatherForecast().subscribe({
      next: (data) => {
        this.weatherData.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load weather data. Make sure the API is running on http://localhost:5000');
        this.loading.set(false);
        console.error('Error loading weather data:', err);
      }
    });
  }

  toggleDarkMode() {
    this.isDarkMode.set(!this.isDarkMode());
    if (typeof window !== 'undefined') {
      localStorage.setItem('darkMode', this.isDarkMode().toString());
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  }

  getWeatherEmoji(summary: string | null): string {
    if (!summary) return '⛅';
    const summaryLower = summary.toLowerCase();
    if (summaryLower.includes('sun') || summaryLower.includes('clear')) return '☀️';
    if (summaryLower.includes('cloud')) return '☁️';
    if (summaryLower.includes('rain')) return '🌧️';
    if (summaryLower.includes('snow')) return '❄️';
    if (summaryLower.includes('storm')) return '⛈️';
    if (summaryLower.includes('wind')) return '💨';
    if (summaryLower.includes('fog')) return '🌫️';
    if (summaryLower.includes('hot') || summaryLower.includes('warm')) return '🌡️';
    if (summaryLower.includes('cold') || summaryLower.includes('freez')) return '🥶';
    return '⛅';
  }
}
