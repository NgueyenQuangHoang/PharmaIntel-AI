// =============================================================================
// chat-connection.ts - Quan ly ket noi SignalR toi /hubs/chat.
//
// - Token JWT truyen qua accessTokenFactory (SignalR tu them ?access_token=...
//   cho WebSocket, va header Authorization cho cac transport fallback).
// - Tu dong reconnect khi rot mang.
// - Base URL suy ra tu VITE_API_URL (bo hau to /api de tro ve goc server).
//
// Yeu cau cai package:  npm i @microsoft/signalr
// =============================================================================
import * as signalR from '@microsoft/signalr'
import { getToken } from '@/features/auth/token-storage'
import type { ChatMessage } from '@/features/chat/chat-types'

// VITE_API_URL thuong la "http://localhost:5292/api" -> hub o goc, bo "/api".
function resolveHubUrl(): string {
  const apiUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5292/api'
  const origin = apiUrl.replace(/\/api\/?$/, '')
  return `${origin}/hubs/chat`
}

export function createChatConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(resolveHubUrl(), {
      accessTokenFactory: () => getToken() ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()
}

// Cac helper mong de goi method tren hub (ten phai khop ChatHub.cs).
export async function joinSession(conn: signalR.HubConnection, sessionId: number) {
  await conn.invoke('JoinSession', sessionId)
}

export async function leaveSession(conn: signalR.HubConnection, sessionId: number) {
  await conn.invoke('LeaveSession', sessionId)
}

export async function sendMessage(conn: signalR.HubConnection, sessionId: number, content: string) {
  await conn.invoke('SendMessage', sessionId, content)
}

export function onReceiveMessage(conn: signalR.HubConnection, handler: (msg: ChatMessage) => void) {
  conn.on('ReceiveMessage', handler)
  return () => conn.off('ReceiveMessage', handler)
}
