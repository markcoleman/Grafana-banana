import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection, APP_INITIALIZER } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { TelemetryService } from './services/telemetry.service';

import { routes } from './app.routes';

// Factory function to initialize telemetry
export function initializeTelemetry(telemetryService: TelemetryService) {
  return () => {
    // Telemetry is initialized in the service constructor
    console.log('Telemetry service initialized');
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
    TelemetryService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeTelemetry,
      deps: [TelemetryService],
      multi: true
    }
  ]
};
