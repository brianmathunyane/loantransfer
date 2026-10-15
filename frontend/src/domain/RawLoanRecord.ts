export interface RawLoanRecord {
  loanId: string;
  startDate: string; // ISO date, e.g. "2019-08-01"
  owner: string;
  rpbByPeriod: Record<string, number | null>; // key: "f202004"
}

export interface RawLoanBatch {
  records: RawLoanRecord[];
}
