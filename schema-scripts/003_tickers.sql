CREATE TABLE tickers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    symbol          VARCHAR(20) NOT NULL,
    name            VARCHAR(200) NOT NULL,
    exchange        VARCHAR(20) NOT NULL,
    currency        CHAR(3) NOT NULL,
    sec_cik         VARCHAR(10),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_tickers_symbol_exchange UNIQUE (symbol, exchange)
);
