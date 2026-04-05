// ─── Shared API response wrappers ────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  requestId?: string
}

export interface WarningResponse {
  warning: string
  requireConfirmation: boolean
}

// ─── Product ──────────────────────────────────────────────────────────────────

export interface Product {
  id: number
  productCode: string
  name: string
  unit: string
  isActive: boolean
  boxQty: number
  moq: number
  safetyStock: number
  averageShipment: number
  categoryCode?: string
  createdAt: string
  updatedAt: string
}

// ─── Supplier & pricing ───────────────────────────────────────────────────────

export interface Supplier {
  id: number
  name: string
  contactInfo: string
}

export interface ProductSupplierPrice {
  id: number
  productId: number
  supplierId: number
  supplierName: string
  unitPrice: number
  currency: string
  minOrderQty: number
  leadTimeDays: number
  effectiveDate: string
  isCurrent: boolean
}

// ─── Purchase order ───────────────────────────────────────────────────────────

export type PurchaseOrderStatus = 'Pending' | 'Confirmed' | 'Received' | 'Cancelled' | '待確認' | '已確認' | '已入荷' | 'キャンセル'

export interface PurchaseOrderItem {
  id: number
  purchaseOrderId: number
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface PurchaseOrder {
  id: number
  orderNumber: string
  supplierId: number
  supplierName: string
  status: PurchaseOrderStatus
  totalAmount: number
  orderDate: string
  createdAt: string
  createdBy: string
  items: PurchaseOrderItem[]
}

// ─── Inventory ────────────────────────────────────────────────────────────────

export interface InventoryRecord {
  id: number
  productId: number
  productName: string
  currentStock: number
  updatedAt: string
}

export type TransactionType = 'StockIn' | 'StockOut'

export interface StockTransaction {
  id: number
  productId: number
  transactionType: TransactionType
  quantity: number
  stockBefore: number
  stockAfter: number
  purchaseOrderId?: number
  transactionDate: string
  operatorAccount: string
  createdAt: string
  remark?: string
}

// ─── Procurement suggestion & settings ───────────────────────────────────────

export interface ProcurementSuggestion {
  id: number
  productId: number
  productName: string
  productCode: string
  currentStock: number
  sixMonthAvgShipment: number
  turnoverMonths: number
  systemSuggestedQty: number
  manualOverrideQty?: number
  isManualOverride: boolean
  dataInsufficient: boolean
  availableMonths?: number
  calculatedAt: string
  // NEW fields
  boxQty: number
  moq: number
  safetyStock: number
  // 第1仕入先（最安値）
  recommendedSupplierName?: string
  recommendedUnitPrice?: number
  recommendedCurrency?: string
  recommendedLeadTimeDays?: number
  supplier1OrderQty?: number
  // 第2仕入先（2番目安値）
  supplier2Name?: string
  supplier2UnitPrice?: number
  supplier2Currency?: string
  supplier2LeadTimeDays?: number
  supplier2OrderQty?: number
  noSupplier: boolean
  // 半年分の月次発注提案
  monthlyOrderSuggestions?: MonthlyOrderSuggestionForSuggestion[]
}

export interface MonthlyOrderSuggestionForSuggestion {
  label: string
  year: number
  month: number
  suggestedQty: number
  estimatedStock: number
}

export interface ProcurementSettings {
  id: number
  defaultTurnoverMonths: number
  updatedBy: string
  updatedAt: string
}

// ─── Demand forecast ─────────────────────────────────────────────────────────

export interface DemandForecast {
  id: number
  productId: number
  forecastMonth: number
  forecastYear: number
  forecastQty: number
  confidenceLower: number
  confidenceUpper: number
  generatedAt: string
}

// ─── Monthly Shipment ─────────────────────────────────────────────────────────

export interface MonthlyShipmentRecord {
  id: number
  productId: number
  year: number
  month: number
  quantity: number
}

export interface MonthlyShipmentResult {
  year: number
  jan: number
  feb: number
  mar: number
  apr: number
  may: number
  jun: number
  jul: number
  aug: number
  sep: number
  oct: number
  nov: number
  dec: number
}

// ─── Warehouse Stock ──────────────────────────────────────────────────────────

export interface WarehouseStock {
  productId: number
  warehouse89: number
  warehouse81: number
  warehouseInspection: number
  warehouse4th: number
  totalStock: number
  unallocatedQty: number
  shippedQty: number
  updatedAt: string
}

// ─── Monthly Inventory ────────────────────────────────────────────────────────

export interface MonthlyInventory {
  id: number
  productId: number
  year: number
  month: number
  orderQty: number
  stockQty: number
  stockAmount: number
  turnoverRate: number
  createdAt: string
}
