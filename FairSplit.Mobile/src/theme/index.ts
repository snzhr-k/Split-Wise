import { colors } from './colors';
import { radius } from './radius';
import { shadows } from './shadows';
import { sizing } from './sizing';
import { spacing } from './spacing';
import { typography } from './typography';

export const theme = {
  colors,
  spacing,
  typography,
  radius,
  shadows,
  sizing,
} as const;

export type Theme = typeof theme;
