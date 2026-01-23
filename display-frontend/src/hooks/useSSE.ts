import { useEffect } from "react";

export function useSSE(
  url: string,
  handlers: Record<string, (data: any) => void>
) {
  useEffect(() => {
    const eventSource = new EventSource(url);

    // Attach handlers for each event type
    Object.entries(handlers).forEach(([eventName, handler]) => {
      eventSource.addEventListener(eventName, (event) => {
        try {
          const parsed = JSON.parse((event as MessageEvent).data);
          handler(parsed);
        } catch {
          handler((event as MessageEvent).data);
        }
      });
    });

    eventSource.onerror = () => {
      // Browser will auto-reconnect, but closing prevents runaway errors
      eventSource.close();
    };

    return () => {
      eventSource.close();
    };
  }, [url, handlers]);
}
