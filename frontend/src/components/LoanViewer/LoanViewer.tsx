import { useTransformedLoans } from '../../hooks/useTransformedLoans';

const currency = new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 });

export function LoanViewer() {
  const { loans, isLoading, errorMessage, refresh } = useTransformedLoans();

  return (
    <section className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-900">
      <header className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-200 px-6 py-5 dark:border-slate-800">
        <div>
          <h2 className="flex items-center gap-2 text-base font-semibold">
            Transformed Loan Data
            {!isLoading && !errorMessage && (
              <span className="rounded-full bg-indigo-100 px-2 py-0.5 text-xs font-medium text-indigo-700 dark:bg-indigo-500/15 dark:text-indigo-300">
                {loans.length}
              </span>
            )}
          </h2>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Ownership periods with remaining principal balance.</p>
        </div>
        <button
          onClick={() => void refresh()}
          disabled={isLoading}
          className="inline-flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600 disabled:cursor-not-allowed disabled:opacity-60 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-200 dark:hover:bg-slate-700"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} className={`size-4 ${isLoading ? 'animate-spin' : ''}`}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h5M20 20v-5h-5M5.6 15a7 7 0 0 0 12.8 1.5M18.4 9A7 7 0 0 0 5.6 7.5" />
          </svg>
          Refresh
        </button>
      </header>

      {isLoading && (
        <div className="space-y-3 p-6" aria-live="polite">
          <span className="sr-only">Loading...</span>
          {Array.from({ length: 4 }, (_, i) => (
            <div key={i} className="h-10 animate-pulse rounded-lg bg-slate-100 dark:bg-slate-800" />
          ))}
        </div>
      )}

      {errorMessage && (
        <div className="p-6">
          <p role="alert" className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700 dark:border-rose-500/30 dark:bg-rose-500/10 dark:text-rose-300">
            {errorMessage}
          </p>
        </div>
      )}

      {!isLoading && !errorMessage && loans.length === 0 && (
        <div className="flex flex-col items-center gap-2 px-6 py-16 text-center">
          <div className="grid size-12 place-items-center rounded-full bg-slate-100 text-slate-400 dark:bg-slate-800">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} className="size-6">
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 7l9-4 9 4-9 4-9-4Zm0 5 9 4 9-4M3 17l9 4 9-4" />
            </svg>
          </div>
          <p className="font-medium">No loan data yet</p>
          <p className="text-sm text-slate-500 dark:text-slate-400">Upload a file above, then refresh to see results.</p>
        </div>
      )}

      {!isLoading && !errorMessage && loans.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500 dark:bg-slate-800/50 dark:text-slate-400">
              <tr>
                <th className="px-6 py-3 font-semibold">Loan ID</th>
                <th className="px-6 py-3 font-semibold">Owner</th>
                <th className="px-6 py-3 font-semibold">Start Date</th>
                <th className="px-6 py-3 font-semibold">RPB by Period</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
              {loans.map((loan) => (
                <tr
                  key={`${loan.loanId}-${loan.owner}-${loan.startDate}`}
                  className="align-top transition-colors hover:bg-slate-50/70 dark:hover:bg-slate-800/40"
                >
                  <td className="whitespace-nowrap px-6 py-4 font-mono text-xs font-medium text-slate-900 dark:text-slate-100">
                    {loan.loanId}
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className="inline-flex items-center rounded-full bg-sky-50 px-2.5 py-1 text-xs font-medium text-sky-700 ring-1 ring-sky-600/20 ring-inset dark:bg-sky-500/10 dark:text-sky-300 dark:ring-sky-400/20">
                      {loan.owner}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-slate-600 tabular-nums dark:text-slate-300">{loan.startDate}</td>
                  <td className="px-6 py-4">
                    <div className="flex flex-wrap gap-1.5">
                      {Object.entries(loan.rpbByPeriod).map(([period, value]) => (
                        <span
                          key={period}
                          className="inline-flex items-center gap-1.5 rounded-md bg-slate-100 px-2 py-1 text-xs dark:bg-slate-800"
                        >
                          <span className="font-mono text-slate-500 dark:text-slate-400">{period}</span>
                          <span className="font-semibold text-slate-900 tabular-nums dark:text-slate-100">{currency.format(value)}</span>
                        </span>
                      ))}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
