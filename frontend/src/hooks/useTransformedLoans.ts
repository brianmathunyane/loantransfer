import { useCallback, useEffect, useState } from 'react';
import { loanTransferApi } from '../services/HttpLoanTransferApi';
import type { LoanOwnershipPeriod } from '../domain/LoanOwnershipPeriod';
import { logger } from '../lib/logger';

export function useTransformedLoans() {
  const [loans, setLoans] = useState<LoanOwnershipPeriod[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);
    try {
      setLoans(await loanTransferApi.getTransformedLoans());
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown fetch error';
      logger.error('Failed to load transformed loans', { message });
      setErrorMessage(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  return { loans, isLoading, errorMessage, refresh };
}
