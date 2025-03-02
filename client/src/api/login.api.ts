import { api } from "src/boot/axios";

export async function loginAPI(casTicket: string): Promise<{ token: string}> {
    return (await api.post('login', { ticket: casTicket })).data;
}