import { type ChangeEvent, type DragEvent, useState } from 'react';
import { useLoanUpload } from '../../hooks/useLoanUpload';

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function LoanUpload() {
  const [file, setFile] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const { status, errorMessage, upload } = useLoanUpload();

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    setFile(e.target.files?.[0] ?? null);
  };

  const handleDrop = (e: DragEvent<HTMLLabelElement>) => {
    e.preventDefault();
    setIsDragging(false);
    const dropped = e.dataTransfer.files?.[0];
    if (dropped) setFile(dropped);
  };

  const handleUpload = () => {
    if (file) void upload(file);
  };

  const isUploading = status === 'uploading';

  return (
    <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900">
      <div className="mb-5">
        <h2 className="text-base font-semibold">Upload Raw Loan Data</h2>
        <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
          Choose a JSON file containing a <code className="rounded bg-slate-100 px-1 py-0.5 text-xs dark:bg-slate-800">records</code> array.
        </p>
      </div>

      <label
        onDragOver={(e) => {
          e.preventDefault();
          setIsDragging(true);
        }}
        onDragLeave={() => setIsDragging(false)}
        onDrop={handleDrop}
        className={`flex cursor-pointer flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed px-6 py-10 text-center transition-colors ${
          isDragging
            ? 'border-indigo-500 bg-indigo-50 dark:bg-indigo-500/10'
            : 'border-slate-300 hover:border-indigo-400 hover:bg-slate-50 dark:border-slate-700 dark:hover:border-indigo-500 dark:hover:bg-slate-800/50'
        }`}
      >
        <div className="grid size-12 place-items-center rounded-full bg-indigo-100 text-indigo-600 dark:bg-indigo-500/15 dark:text-indigo-400">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} className="size-6">
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 16V4m0 0-4 4m4-4 4 4M4 16v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2" />
          </svg>
        </div>
        {file ? (
          <div>
            <p className="font-medium text-slate-900 dark:text-slate-100">{file.name}</p>
            <p className="text-sm text-slate-500 dark:text-slate-400">{formatBytes(file.size)} · click to change</p>
          </div>
        ) : (
          <div>
            <p className="font-medium text-slate-900 dark:text-slate-100">
              <span className="text-indigo-600 dark:text-indigo-400">Click to browse</span> or drag and drop
            </p>
            <p className="text-sm text-slate-500 dark:text-slate-400">JSON files only</p>
          </div>
        )}
        <input type="file" accept=".json" onChange={handleFileChange} className="sr-only" />
      </label>

      <div className="mt-5 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="min-h-5 text-sm">
          {status === 'success' && (
            <p role="status" className="flex items-center gap-2 font-medium text-emerald-600 dark:text-emerald-400">
              <svg viewBox="0 0 20 20" fill="currentColor" className="size-5 shrink-0">
                <path fillRule="evenodd" d="M10 18a8 8 0 1 0 0-16 8 8 0 0 0 0 16Zm3.86-9.86a.75.75 0 0 0-1.22-.88l-3.24 4.5-1.6-1.6a.75.75 0 1 0-1.06 1.06l2.22 2.22a.75.75 0 0 0 1.14-.09l3.76-5.21Z" clipRule="evenodd" />
              </svg>
              Upload successful — processing in background.
            </p>
          )}
          {status === 'error' && (
            <p role="alert" className="flex items-center gap-2 font-medium text-rose-600 dark:text-rose-400">
              <svg viewBox="0 0 20 20" fill="currentColor" className="size-5 shrink-0">
                <path fillRule="evenodd" d="M18 10a8 8 0 1 1-16 0 8 8 0 0 1 16 0Zm-8-5a.75.75 0 0 1 .75.75v4.5a.75.75 0 0 1-1.5 0v-4.5A.75.75 0 0 1 10 5Zm0 10a1 1 0 1 0 0-2 1 1 0 0 0 0 2Z" clipRule="evenodd" />
              </svg>
              {errorMessage}
            </p>
          )}
        </div>

        <button
          onClick={handleUpload}
          disabled={!file || isUploading}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white shadow-sm shadow-indigo-600/30 transition hover:bg-indigo-500 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:cursor-not-allowed disabled:bg-slate-300 disabled:shadow-none dark:disabled:bg-slate-700 dark:disabled:text-slate-400"
        >
          {isUploading && (
            <svg viewBox="0 0 24 24" fill="none" className="size-4 animate-spin">
              <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" className="opacity-25" />
              <path d="M22 12a10 10 0 0 0-10-10" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
            </svg>
          )}
          {isUploading ? 'Uploading...' : 'Upload'}
        </button>
      </div>
    </section>
  );
}
