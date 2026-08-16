CREATE TABLE holdings (
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE RESTRICT,
    shares          NUMERIC(20, 8) NOT NULL DEFAULT 0,
    avg_cost        NUMERIC(18, 6) NOT NULL DEFAULT 0,
    realized_pnl    NUMERIC(20, 6) NOT NULL DEFAULT 0,
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, ticker_id)
);
