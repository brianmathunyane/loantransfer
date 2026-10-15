import { useCallback, useState } from 'react';
import { loanTransferApi } from '../services/HttpLoanTransferApi';
import type { RawLoanBatch } from '../domain/RawLoanRecord';
import { logger } from '../lib/logger';

type UploadStatus = 'idle' | 'uploading' | 'success' | 'error';

export function useLoanUpload() {
  const [status, setStatus] = useState<UploadStatus>('idle');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const upload = useCallback(async (file: File) => {
    setStatus('uploading');
    setErrorMessage(null);

    try {
      const text = await file.text();
      const batch = JSON.parse(text) as RawLoanBatch;

      if (!Array.isArray(batch.records)) {
        throw new Error('File must contain a "records" array.');
      }

      await loanTransferApi.submitLoanTransfers(batch);
      setStatus('success');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Unknown upload error';
      logger.error('Upload failed', { message });
      setErrorMessage(message);
      setStatus('error');
    }
  }, []);

  return { status, errorMessage, upload };
}
