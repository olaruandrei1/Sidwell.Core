CREATE TABLE IF NOT EXISTS dividend_yield_history (
    id          BIGSERIAL PRIMARY KEY,
    symbol      TEXT        NOT NULL,
    year        SMALLINT    NOT NULL,
    yield_pct   NUMERIC(8,4) NOT NULL,
    source      TEXT        NOT NULL DEFAULT 'manual',
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_dividend_yield_history_symbol_year UNIQUE (symbol, year)
);

CREATE INDEX IF NOT EXISTS idx_dividend_yield_history_symbol ON dividend_yield_history (symbol);
