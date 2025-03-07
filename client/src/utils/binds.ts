import type { QBtnProps, QInputProps } from "quasar";

export const buttonRegularBind: QBtnProps = {
    'noCaps': true,
    color: 'secondary',
    size: 'md'
}
export const inputRegularBind: Omit<QInputProps, 'modelValue'> = {
    filled: true
}
