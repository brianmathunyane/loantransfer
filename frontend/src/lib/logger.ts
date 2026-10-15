type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface Logger {
  debug(msg: string, meta?: Record<string, unknown>): void;
  info(msg: string, meta?: Record<string, unknown>): void;
  warn(msg: string, meta?: Record<string, unknown>): void;
  error(msg: string, meta?: Record<string, unknown>): void;
}

function log(level: LogLevel, msg: string, meta?: Record<string, unknown>) {
  const entry = { level, msg, time: new Date().toISOString(), ...meta };
  // eslint-disable-next-line no-console
  console[level === 'debug' ? 'log' : level](entry);
}

export const logger: Logger = {
  debug: (m, meta) => log('debug', m, meta),
  info: (m, meta) => log('info', m, meta),
  warn: (m, meta) => log('warn', m, meta),
  error: (m, meta) => log('error', m, meta),
};
