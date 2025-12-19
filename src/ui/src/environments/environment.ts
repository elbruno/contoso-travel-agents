export const environment = {
  production: true,
  // Use build-time NG_API_URL when provided, otherwise default to an empty
  // string so relative API requests go to the same origin (e.g. /api/chat).
  apiServerUrl: (typeof import.meta !== 'undefined' && (import.meta as any).env?.NG_API_URL) || ''
};
