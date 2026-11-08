/* Sort options for the opportunities list. The `value` of each option is sent
   verbatim as `sortBy` to the backend, so it MUST match the values handled in
   GetOpportunitiesQueryHandler: "date" | "duration" | "urgent" (empty = default
   relevance). Legacy values "soonest"/"mostNeeded" were never supported. */
export const OPPORTUNITY_SORT_OPTIONS = [
  { value: '', label: 'Best match' },
  { value: 'date', label: 'Soonest' },
  { value: 'duration', label: 'Shortest duration' },
  { value: 'urgent', label: 'Urgent first' },
];

/* The exact set the backend understands (excluding the empty default). */
export const OPPORTUNITY_SORT_VALUES = ['date', 'duration', 'urgent'];

export const DEFAULT_OPPORTUNITIES_PAGE_SIZE = 24;

export function normalizePagedOpportunities(data, fallbackPageSize = DEFAULT_OPPORTUNITIES_PAGE_SIZE) {
  if (Array.isArray(data)) {
    return {
      items: data,
      page: 1,
      pageSize: fallbackPageSize,
      totalCount: data.length,
      totalPages: data.length ? 1 : 0,
    };
  }

  const items = data?.items || [];
  const pageSize = data?.pageSize || fallbackPageSize;
  const totalCount = data?.totalCount ?? items.length;
  return {
    items,
    page: data?.page || 1,
    pageSize,
    totalCount,
    totalPages: data?.totalPages ?? (pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0),
  };
}
