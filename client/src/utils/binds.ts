import type { QDialogProps, QInputProps } from 'quasar'

export const dialogDefaultBind: Omit<QDialogProps, 'modelValue'> = {
  backdropFilter: 'blur(6px)',
}
export const inputDefaultBind: Omit<QInputProps, 'modelValue'> = {
  dark: true,
  square: true,
  outlined: true,
  color: 'secondary',
  labelColor: 'secondary',
}
