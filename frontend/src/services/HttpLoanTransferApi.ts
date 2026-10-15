import type { LoanTransferApi } from './LoanTransferApi';
import type { RawLoanBatch } from '../domain/RawLoanRecord';
import type { LoanOwnershipPeriod } from '../domain/LoanOwnershipPeriod';
import { logger } from '../lib/logger';

export class ApiError extends Error {
  readonly status?: number;

  constructor(message: string, status?: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export class HttpLoanTransferApi implements LoanTransferApi {
  private readonly baseUrl: string;

  constructor(baseUrl: string = import.meta.env.VITE_API_BASE_URL ?? '/api') {
    this.baseUrl = baseUrl;
  }

  async submitLoanTransfers(batch: RawLoanBatch): Promise<void> {
    const res = await fetch(`${this.baseUrl}/loan-transfers`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(batch),
    });

    if (!res.ok) {
      const detail = await res.text().catch(() => '');
      logger.error('Loan transfer submission failed', { status: res.status, detail });
      throw new ApiError(`Upload failed (${res.status})`, res.status);
    }
  }

  async getTransformedLoans(): Promise<LoanOwnershipPeriod[]> {
    const res = await fetch(`${this.baseUrl}/transformed-loans`);

    if (!res.ok) {
      logger.error('Failed to fetch transformed loans', { status: res.status });
      throw new ApiError(`Fetch failed (${res.status})`, res.status);
    }

    return res.json() as Promise<LoanOwnershipPeriod[]>;
  }
}

export const loanTransferApi: LoanTransferApi = new HttpLoanTransferApi();
