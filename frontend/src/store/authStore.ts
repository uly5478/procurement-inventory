import { create } from 'zustand'

interface User {
  account: string
  displayName: string
}

interface AuthState {
  token: string | null
  user: User | null
  setToken: (token: string, user: User) => void
  clearToken: () => void
}

const useAuthStore = create<AuthState>((set) => ({
  token: localStorage.getItem('token'),
  user: null,

  setToken: (token, user) => {
    localStorage.setItem('token', token)
    set({ token, user })
  },

  clearToken: () => {
    localStorage.removeItem('token')
    set({ token: null, user: null })
  },
}))

export default useAuthStore
