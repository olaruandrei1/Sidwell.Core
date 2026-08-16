CREATE TABLE price_history (
    ticker_id   UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    date        DATE NOT NULL,
    open        NUMERIC(18, 6) NOT NULL,
    high        NUMERIC(18, 6) NOT NULL,
    low         NUMERIC(18, 6) NOT NULL,
    close       NUMERIC(18, 6) NOT NULL,
    volume      BIGINT NOT NULL,
    source      VARCHAR(30) NOT NULL,
    PRIMARY KEY (ticker_id, date)
);

CREATE INDEX idx_price_history_ticker_date_desc ON price_history (ticker_id, date DESC);
