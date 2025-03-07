export interface ILogComponentInfo {
  title: string
  date: string
  note?: string
  cores: {
    type: 'fixes' | 'changes'
    list: {
      text: string
      icon?: 'alert' | 'new'
      priority?: number
    }[]
  }[]
}

export const allLogs: ILogComponentInfo[] = [
  {
    title: 'Mise en production',
    date: '???',
    cores: [],
  },
]
