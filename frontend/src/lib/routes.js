import { UserRole } from './enums.js';

/* Returns the home/dashboard route for a given role. */
export function dashboardPath(role) {
  switch (role) {
    case UserRole.Organization: return '/org';
    case UserRole.Admin: return '/admin';
    case UserRole.Volunteer: return '/app';
    default: return '/login';
  }
}
