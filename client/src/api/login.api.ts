import { api } from "src/boot/axios";

export async function loginAPI(casTicket: string): Promise<{ token: string }> {
    return (await api.post('login', { casTicket: casTicket, service: window.location.origin })).data;
}
export async function fakeLoginAPI(idRes: string): Promise<{ token: string }> {
    return (await api.post('fake-login', { idRes })).data;
}
