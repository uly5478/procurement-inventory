import client from './client'
import type { ApiResponse, ProcurementSuggestion, ProcurementSettings } from '../types'

export async function getSuggestions(useForecast = false) {
  const res = await client.get<ApiResponse<ProcurementSuggestion[]>>('/procurement/suggestions', {
    params: { useForecast },
  })
  return res.data.data
}

export async function manualOverride(productId: number, qty: number) {
  const res = await client.put<ApiResponse<ProcurementSuggestion>>(
    `/procurement/suggestions/${productId}`,
    { manualOverrideQty: qty },
  )
  return res.data.data
}

export async function getSettings() {
  const res = await client.get<ApiResponse<ProcurementSettings>>('/procurement/settings')
  return res.data.data
}

export async function updateSettings(dto: { defaultTurnoverMonths: number }) {
  const res = await client.put<ApiResponse<ProcurementSettings>>('/procurement/settings', dto)
  return res.data.data
}

export async function resetOverride(productId: number) {
  const res = await client.delete<ApiResponse<ProcurementSuggestion>>(
    `/procurement/suggestions/${productId}/override`
  )
  return res.data.data
}