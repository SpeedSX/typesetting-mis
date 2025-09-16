// Organization-related constants shared across components

export const TIMEZONES = [
  'UTC',
  'America/New_York',
  'America/Chicago',
  'America/Denver',
  'America/Los_Angeles',
  'Europe/London',
  'Europe/Paris',
  'Europe/Berlin',
  'Asia/Tokyo',
  'Asia/Shanghai',
  'Australia/Sydney',
] as const;

export const CURRENCIES = [
  'USD',
  'EUR',
  'GBP',
  'JPY',
  'CAD',
  'AUD',
  'CHF',
  'CNY',
] as const;

export const SUBSCRIPTION_PLANS = [
  'Basic',
  'Professional',
  'Enterprise',
] as const;

// Type definitions for better type safety
export type Timezone = typeof TIMEZONES[number];
export type Currency = typeof CURRENCIES[number];
export type SubscriptionPlan = typeof SUBSCRIPTION_PLANS[number];
