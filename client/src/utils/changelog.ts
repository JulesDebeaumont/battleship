export interface ILogComponentInfo {
  title: string
  date: string
  note?: string
  cores: {
    type: 'fixes' | 'changes' | 'add'
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
    cores: [
      {
        type: 'add',
        list: [
          {
            text: "Système d'authentification",
          },
          {
            text: 'Système de bataille',
          },
          {
            text: 'Système de mode spectateur',
          },
          {
            text: 'Système de classement',
          },
          {
            text: 'Système de profil',
          },
        ],
      },
    ],
    note: 'OST℠',
  },
]
