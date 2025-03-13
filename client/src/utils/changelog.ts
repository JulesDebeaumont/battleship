export interface ILogComponentInfo {
  title: string
  date: string
  note?: string
  cores: {
    type: 'fixes' | 'changes'
    list: {
      text: string
      icon?: 'alert' | 'new'
    }[]
  }[]
}

export const allLogs: ILogComponentInfo[] = [
  {
    title: 'Mise en production',
    date: '13/03/2025',
    cores: [],
    note: 'OST©',
  },
]
