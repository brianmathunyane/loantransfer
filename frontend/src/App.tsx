import { ErrorBoundary } from './components/ErrorBoundary';
import { LoanUpload } from './components/LoanUpload/LoanUpload';
import { LoanViewer } from './components/LoanViewer/LoanViewer';

export function App() {
  return (
    <ErrorBoundary>
      <div className="relative min-h-screen overflow-hidden">
        <div
          aria-hidden="true"
          className="pointer-events-none absolute inset-x-0 -top-40 -z-10 h-[28rem] bg-gradient-to-br from-indigo-200/70 via-sky-100/60 to-transparent blur-3xl dark:from-indigo-900/40 dark:via-sky-900/20"
        />

        <header className="border-b border-slate-200/70 bg-white/70 backdrop-blur dark:border-slate-800 dark:bg-slate-900/60">
          <div className="mx-auto flex max-w-6xl items-center gap-3 px-4 py-4 sm:px-6">
            <div className="grid size-9 place-items-center rounded-xl bg-gradient-to-br from-indigo-500 to-sky-500 text-white shadow-lg shadow-indigo-500/30">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className="size-5">
                <path strokeLinecap="round" strokeLinejoin="round" d="M7 7h11l-3-3M17 17H6l3 3" />
              </svg>
            </div>
            <div>
              <h1 className="text-lg font-semibold tracking-tight">Loan Transfer Processor</h1>
              <p className="text-xs text-slate-500 dark:text-slate-400">
                Upload raw loan data and review ownership periods
              </p>
            </div>
          </div>
        </header>

        <main className="mx-auto grid max-w-6xl gap-6 px-4 py-8 sm:px-6 lg:py-10">
          <LoanUpload />
          <LoanViewer />
        </main>
      </div>
    </ErrorBoundary>
  );
}
