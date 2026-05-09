/**
 * Lightweight toast/notification system for MVP.
 * No dependencies, just simple in-app alerts.
 */

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export type Toast = {
  id: string;
  type: ToastType;
  message: string;
  duration?: number; // milliseconds, null = persistent
};

export type ToastListener = (toasts: Toast[]) => void;

/**
 * Simple toast service - emit events for any screen/component to listen to.
 * This is MVP-friendly: no context providers required, works from anywhere.
 */
class ToastService {
  private toasts: Toast[] = [];
  private listeners: Set<ToastListener> = new Set();
  private nextId = 0;

  /**
   * Show a toast message.
   */
  show(type: ToastType, message: string, duration: number = 3000): string {
    const id = `toast-${this.nextId++}`;

    const toast: Toast = {
      id,
      type,
      message,
      duration: duration > 0 ? duration : undefined,
    };

    this.toasts = [toast, ...this.toasts];
    this.notifyListeners();

    // Auto-dismiss if duration is set
    if (duration > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, duration);
    }

    return id;
  }

  /**
   * Show success toast.
   */
  success(message: string, duration?: number): string {
    return this.show('success', message, duration);
  }

  /**
   * Show error toast.
   */
  error(message: string, duration?: number): string {
    return this.show('error', message, duration ?? 4000);
  }

  /**
   * Show warning toast.
   */
  warning(message: string, duration?: number): string {
    return this.show('warning', message, duration ?? 3500);
  }

  /**
   * Show info toast.
   */
  info(message: string, duration?: number): string {
    return this.show('info', message, duration);
  }

  /**
   * Dismiss a specific toast by ID.
   */
  dismiss(id: string): void {
    this.toasts = this.toasts.filter(toast => toast.id !== id);
    this.notifyListeners();
  }

  /**
   * Dismiss all toasts.
   */
  dismissAll(): void {
    this.toasts = [];
    this.notifyListeners();
  }

  /**
   * Get current toasts.
   */
  getToasts(): Toast[] {
    return this.toasts;
  }

  /**
   * Subscribe to toast changes.
   * Returns unsubscribe function.
   */
  subscribe(listener: ToastListener): () => void {
    this.listeners.add(listener);

    return () => {
      this.listeners.delete(listener);
    };
  }

  private notifyListeners(): void {
    this.listeners.forEach(listener => {
      listener([...this.toasts]);
    });
  }
}

/**
 * Singleton toast service instance.
 */
export const toastService = new ToastService();
