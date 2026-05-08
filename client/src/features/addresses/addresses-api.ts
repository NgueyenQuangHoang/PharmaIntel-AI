// =============================================================================
// Addresses API - tat ca yeu cau JWT
// =============================================================================
import { httpClient } from '@/services/http-client'
import type { AddressCreateRequest, AddressDto, PagedResult } from './types'

export const addressesApi = {
  listMy: async (): Promise<PagedResult<AddressDto>> => {
    const res = await httpClient.get<PagedResult<AddressDto>>('/addresses/my', {
      params: { page: 1, pageSize: 50 },
    })
    return res.data
  },
  create: async (req: AddressCreateRequest): Promise<AddressDto> => {
    const res = await httpClient.post<AddressDto>('/addresses', req)
    return res.data
  },
}
