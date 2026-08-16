CREATE TABLE insider_transactions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    insider         VARCHAR(200) NOT NULL,
    type            VARCHAR(20) NOT NULL, -- 'BUY' | 'SELL' | ...
    shares          NUMERIC(20, 4) NOT NULL,
    price           NUMERIC(18, 6),
    tx_date         DATE NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_insider_transactions_ticker_tx_date ON insider_transactions (ticker_id, tx_date DESC);
