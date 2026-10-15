export interface LoanOwnershipPeriod {
  loanId: string;
  owner: string;
  startDate: string;
  rpbByPeriod: Record<string, number>; // only the periods this owner is responsible for
}
