import type { RawLoanBatch } from '../domain/RawLoanRecord';
import type { LoanOwnershipPeriod } from '../domain/LoanOwnershipPeriod';

export interface LoanTransferApi {
  submitLoanTransfers(batch: RawLoanBatch): Promise<void>;
  getTransformedLoans(): Promise<LoanOwnershipPeriod[]>;
}
