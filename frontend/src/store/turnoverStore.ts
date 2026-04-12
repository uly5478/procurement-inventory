import { create } from 'zustand'

interface TurnoverState {
  turnover: number
  initialized: boolean  // 追蹤是否已從後端初始化
  setTurnover: (turnover: number) => void
  initializeFromBackend: (defaultTurnover: number) => void
}

const useTurnoverStore = create<TurnoverState>((set, get) => ({
  turnover: 2.5, // 初始值，會被後端覆蓋
  initialized: false,
  setTurnover: (turnover) => set({ turnover, initialized: true }),  // 使用者手動設定時標記為已初始化
  initializeFromBackend: (defaultTurnover) => {
    // 只有在還沒初始化時才從後端載入
    if (!get().initialized) {
      set({ turnover: defaultTurnover, initialized: true })
    }
  },
}))

export default useTurnoverStore
