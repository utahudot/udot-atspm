import { create } from 'zustand'

interface SidebarStore {
  isSidebarOpen: boolean
  toggleSidebar: () => void
  closeSideBar: () => void
  openSideBar: () => void
}
export const useSidebarStore = create<SidebarStore>((set) => ({
  isSidebarOpen: false,

  toggleSidebar: () =>
    set((state) => {
      return { isSidebarOpen: !state.isSidebarOpen }
    }),

  closeSideBar: () => set({ isSidebarOpen: false }),

  openSideBar: () => set({ isSidebarOpen: true }),
}))
