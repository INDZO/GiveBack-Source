/* Domain-grouped API service modules. Components call these instead of
   issuing scattered fetch() calls. Each maps 1:1 to a backend controller. */
import { api } from './client';

export const authApi = {
  login: (email, password) => api.post('/auth/login', { email, password }, { auth: false }),
  registerVolunteer: (payload) => api.post('/auth/register/volunteer', payload, { auth: false }),
  registerOrganization: (payload) => api.post('/auth/register/organization', payload, { auth: false }),
  // Logout must succeed even when the access token is already expired, so it is
  // sent anonymously (the refresh token in the body identifies the session).
  logout: (refreshToken) => api.post('/auth/logout', { refreshToken }, { auth: false }),
};

export const lookupsApi = {
  causes: () => api.get('/lookups/causes', { auth: false }),
  skills: () => api.get('/lookups/skills', { auth: false }),
};

export const opportunitiesApi = {
  list: (filters) => api.get('/opportunities', { auth: false, query: filters }),
  // Detail is [AllowAnonymous] but enforces a private-visibility access policy.
  // Send the token when the user is logged in so invited/authorized volunteers
  // can open private opportunities; anonymous users still browse public ones
  // (the client only attaches the Authorization header when a token exists).
  get: (id) => api.get(`/opportunities/${id}`),
  urgent: () => api.get('/opportunities/urgent', { auth: false }),
  matchingMe: () => api.get('/opportunities/matching-me'),
  create: (dto) => api.post('/opportunities', dto),
  update: (id, dto) => api.put(`/opportunities/${id}`, dto),
  remove: (id) => api.del(`/opportunities/${id}`),
  cancel: (id) => api.patch(`/opportunities/${id}/cancel`),
  complete: (id) => api.patch(`/opportunities/${id}/complete`),
  invite: (id, volunteerId) => api.post(`/opportunities/${id}/invite`, { volunteerId }),
  myInvitations: () => api.get('/opportunities/invitations/my'),
  acceptInvitation: (id) => api.patch(`/opportunities/invitations/${id}/accept`),
  declineInvitation: (id) => api.patch(`/opportunities/invitations/${id}/decline`),
};

export const applicationsApi = {
  apply: (opportunityId, message) => api.post(`/opportunities/${opportunityId}/apply`, { message }),
  applyTeam: (opportunityId, teamId, message) =>
    api.post(`/opportunities/${opportunityId}/apply-team`, { teamId, message }),
  mine: () => api.get('/applications/my'),
  withdraw: (id) => api.patch(`/applications/${id}/withdraw`),
  forOrganization: () => api.get('/organizations/applications'),
  accept: (id) => api.patch(`/applications/${id}/accept`),
  reject: (id) => api.patch(`/applications/${id}/reject`),
  attended: (id) => api.patch(`/applications/${id}/attended`),
  noShow: (id) => api.patch(`/applications/${id}/no-show`),
};

export const volunteersApi = {
  me: () => api.get('/volunteers/me'),
  updateMe: (cmd) => api.put('/volunteers/me', cmd),
  updateSkills: (skillIds) => api.put('/volunteers/me/skills', { skillIds }),
  updateInterests: (causeIds) => api.put('/volunteers/me/interests', { causeIds }),
  updateAvailability: (availability) => api.put('/volunteers/me/availability', { availability }),
  hours: () => api.get('/volunteers/me/hours'),
  badges: () => api.get('/volunteers/me/badges'),
  certificates: () => api.get('/volunteers/me/certificates'),
  annualReport: (year) => api.get('/volunteers/me/annual-report', { query: { year } }),
  annualImpact: (year) => api.get('/volunteers/me/annual-impact', { query: { year } }),
  search: (query) => api.get('/volunteers/search', { query }),
};

export const organizationsApi = {
  me: () => api.get('/organizations/me'),
  updateMe: (cmd) => api.put('/organizations/me', cmd),
  statistics: () => api.get('/organizations/me/statistics'),
  myOpportunities: () => api.get('/organizations/me/opportunities'),
  impact: () => api.get('/organizations/me/impact'),
};

export const hourLogsApi = {
  create: (cmd) => api.post('/hour-logs', cmd),
  mine: () => api.get('/hour-logs/my'),
  pending: () => api.get('/organizations/hour-logs/pending'),
  approve: (id) => api.patch(`/hour-logs/${id}/approve`),
  correct: (id, approvedHours) => api.patch(`/hour-logs/${id}/correct`, { approvedHours }),
  reject: (id) => api.patch(`/hour-logs/${id}/reject`),
};

export const certificatesApi = {
  generate: (cmd) => api.post('/certificates/generate', cmd),
  mine: () => api.get('/certificates/my'),
  downloadUrl: (id) => `${api.baseUrl}/certificates/${id}/download`,
  download: (id) => api.raw(`/certificates/${id}/download`),
};

export const impactApi = {
  // Same as opportunity detail: attach the token when present so private
  // opportunity reports are reachable by authorized users.
  opportunityReport: (id) => api.get(`/opportunities/${id}/impact-report`),
  createReport: (id, payload) => api.post(`/opportunities/${id}/impact-report`, payload),
};

export const badgesApi = {
  all: () => api.get('/badges', { auth: false }),
};

export const leaderboardApi = {
  monthly: (month, year) => api.get('/leaderboard/monthly', { auth: false, query: { month, year } }),
  yearly: (year) => api.get('/leaderboard/yearly', { auth: false, query: { year } }),
};

export const notificationsApi = {
  list: () => api.get('/notifications'),
  markRead: (id) => api.patch(`/notifications/${id}/read`),
  markAllRead: () => api.patch('/notifications/read-all'),
};

export const teamsApi = {
  create: (name) => api.post('/teams', { name }),
  mine: () => api.get('/teams/my'),
  addMember: (id, volunteerId) => api.post(`/teams/${id}/members`, { volunteerId }),
  confirm: (id, memberId) => api.patch(`/teams/${id}/members/${memberId}/confirm`),
  decline: (id, memberId) => api.patch(`/teams/${id}/members/${memberId}/decline`),
};

export const adminApi = {
  pendingOrganizations: () => api.get('/admin/organizations/pending'),
  verifyOrganization: (id) => api.patch(`/admin/organizations/${id}/verify`),
  rejectOrganization: (id, reason) => api.patch(`/admin/organizations/${id}/reject`, { reason }),
  statistics: () => api.get('/admin/statistics'),
  users: () => api.get('/admin/users'),
  activateUser: (id) => api.patch(`/admin/users/${id}/activate`),
  deactivateUser: (id) => api.patch(`/admin/users/${id}/deactivate`),
  causes: () => api.get('/admin/causes'),
  createCause: (name) => api.post('/admin/causes', { name }),
  updateCause: (id, name) => api.put(`/admin/causes/${id}`, { name }),
  deleteCause: (id) => api.del(`/admin/causes/${id}`),
  opportunities: () => api.get('/admin/opportunities'),
  cancelOpportunity: (id) => api.patch(`/admin/opportunities/${id}/cancel`),
};
