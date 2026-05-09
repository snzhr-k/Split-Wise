import { useEffect, useState } from 'react';

import { toastService, type Toast } from '../services/toastService';

/**
 * Hook to subscribe to toast notifications and render them.
 * Usage:
 *
 * function ToastContainer() {
 *   const toasts = useToasts();
 *   return (
 *     <View>
 *       {toasts.map(toast => (
 *         <Toast key={toast.id} toast={toast} onDismiss={() => toastService.dismiss(toast.id)} />
 *       ))}
 *     </View>
 *   );
 * }
 */
export function useToasts(): Toast[] {
  const [toasts, setToasts] = useState<Toast[]>(() => toastService.getToasts());

  useEffect(() => {
    const unsubscribe = toastService.subscribe(setToasts);
    return unsubscribe;
  }, []);

  return toasts;
}
