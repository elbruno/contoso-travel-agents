export const environment = {
  production: false,
  // Use the current host so other devices on the LAN can reach the API
  // when the UI is served from this machine (e.g. http://192.168.1.41:4200)
  apiServerUrl:
    (typeof window !== 'undefined' && `http://${window.location.hostname}:4000`) ||
    'http://localhost:4000',
};
