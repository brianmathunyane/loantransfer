import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useLoanUpload } from './useLoanUpload';
import { loanTransferApi } from '../services/HttpLoanTransferApi';

vi.mock('../services/HttpLoanTransferApi', () => ({
  loanTransferApi: { submitLoanTransfers: vi.fn() },
}));

describe('useLoanUpload', () => {
  beforeEach(() => vi.clearAllMocks());

  it('transitions to success on a valid upload', async () => {
    const { result } = renderHook(() => useLoanUpload());
    const file = new File([JSON.stringify({ records: [] })], 'data.json', {
      type: 'application/json',
    });

    await act(async () => {
      await result.current.upload(file);
    });

    expect(loanTransferApi.submitLoanTransfers).toHaveBeenCalledOnce();
    await waitFor(() => expect(result.current.status).toBe('success'));
  });

  it('transitions to error on malformed JSON', async () => {
    const { result } = renderHook(() => useLoanUpload());
    const file = new File(['not json'], 'bad.json', { type: 'application/json' });

    await act(async () => {
      await result.current.upload(file);
    });

    expect(result.current.status).toBe('error');
    expect(result.current.errorMessage).toBeTruthy();
  });

  it('transitions to error when the records field is missing', async () => {
    const { result } = renderHook(() => useLoanUpload());
    const file = new File([JSON.stringify({})], 'bad.json', {
      type: 'application/json',
    });

    await act(async () => {
      await result.current.upload(file);
    });

    expect(result.current.status).toBe('error');
    expect(loanTransferApi.submitLoanTransfers).not.toHaveBeenCalled();
  });
});
