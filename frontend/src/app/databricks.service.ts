import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BananaProduction {
  region: string;
  year: number;
  month: number;
  tonsProduced: number;
  averageQualityScore: number;
  varietyName: string;
  exportPercentage: number;
}

export interface BananaSalesData {
  country: string;
  totalSales: number;
  totalBunches: number;
  averagePrice: number;
  marketShare: number;
}

export interface BananaAnalyticsSummary {
  totalProductionTons: number;
  globalAverageQuality: number;
  totalRevenue: number;
  countriesServed: number;
  topProducingRegion: string;
  mostPopularVariety: string;
}

export interface BananaAnalytics {
  generatedAt: string;
  dataSource: string;
  productions: BananaProduction[];
  sales: BananaSalesData[];
  summary: BananaAnalyticsSummary;
}

@Injectable({
  providedIn: 'root'
})
export class DatabricksService {
  private http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/databricks';

  getBananaAnalytics(): Observable<BananaAnalytics> {
    return this.http.get<BananaAnalytics>(`${this.apiUrl}/banana-analytics`);
  }

  getProductionData(year: number): Observable<BananaProduction[]> {
    return this.http.get<BananaProduction[]>(`${this.apiUrl}/production/${year}`);
  }

  getSalesData(region?: string): Observable<BananaSalesData[]> {
    const url = region 
      ? `${this.apiUrl}/sales?region=${encodeURIComponent(region)}`
      : `${this.apiUrl}/sales`;
    return this.http.get<BananaSalesData[]>(url);
  }
}
