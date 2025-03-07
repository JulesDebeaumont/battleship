export const isRequiredRule = (val: string | number) => !!val || 'Ce champ est requis'
export const maxLengthRule = (length: number) => (val: string) =>
  val.length <= length || `${length} caractères max.`
export const minLengthRule = (length: number) => (val: string) =>
  val.length >= length || `${length} caractères min.`
