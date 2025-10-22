import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TelemetryService {
  constructor() {
    this.initializeTelemetry();
  }

  private async initializeTelemetry(): Promise<void> {
    try {
      // Dynamically import OpenTelemetry modules to avoid build issues
      const { WebTracerProvider } = await import('@opentelemetry/sdk-trace-web');
      const ResourcesModule = await import('@opentelemetry/resources');
      const SemConvModule = await import('@opentelemetry/semantic-conventions');
      const { registerInstrumentations } = await import('@opentelemetry/instrumentation');
      const { DocumentLoadInstrumentation } = await import('@opentelemetry/instrumentation-document-load');
      const { UserInteractionInstrumentation } = await import('@opentelemetry/instrumentation-user-interaction');
      const { FetchInstrumentation } = await import('@opentelemetry/instrumentation-fetch');
      const { XMLHttpRequestInstrumentation } = await import('@opentelemetry/instrumentation-xml-http-request');
      const { OTLPTraceExporter } = await import('@opentelemetry/exporter-trace-otlp-http');
      const { BatchSpanProcessor } = await import('@opentelemetry/sdk-trace-base');
      const { ZoneContextManager } = await import('@opentelemetry/context-zone');

      // Create a resource describing this service
      const Resource = (ResourcesModule as any).Resource;
      const resource = Resource.default().merge(
        new Resource({
          [(SemConvModule as any).SEMRESATTRS_SERVICE_NAME]: 'grafana-banana-frontend',
          [(SemConvModule as any).SEMRESATTRS_SERVICE_VERSION]: '1.0.0',
          'deployment.environment': 'development',
        })
      );

      // Configure the OTLP exporter to send traces to Tempo via the backend
      const exporter = new OTLPTraceExporter({
        url: 'http://localhost:4318/v1/traces', // OTLP HTTP endpoint
      });

      // Configure the tracer provider with span processor in config
      const provider = new WebTracerProvider({
        resource: resource,
        spanProcessors: [new BatchSpanProcessor(exporter)]
      });

      // Register the provider
      provider.register({
        contextManager: new ZoneContextManager(),
      });

      // Register instrumentations for automatic tracing
      registerInstrumentations({
        instrumentations: [
          new DocumentLoadInstrumentation(),
          new UserInteractionInstrumentation({
            eventNames: ['click', 'submit'],
          }),
          new FetchInstrumentation({
            propagateTraceHeaderCorsUrls: [
              'http://localhost:5000',
              /localhost:5000/,
            ],
            clearTimingResources: true,
          }),
          new XMLHttpRequestInstrumentation({
            propagateTraceHeaderCorsUrls: [
              'http://localhost:5000',
              /localhost:5000/,
            ],
          }),
        ],
      });

      console.log('OpenTelemetry initialized for Grafana-banana frontend');
    } catch (error) {
      console.error('Failed to initialize OpenTelemetry:', error);
    }
  }

  // Method to track custom events
  public trackEvent(eventName: string, properties?: Record<string, any>): void {
    console.log('Track event:', eventName, properties);
  }

  // Method to track errors
  public trackError(error: Error, context?: Record<string, any>): void {
    console.error('Track error:', error, context);
  }
}
