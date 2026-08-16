CREATE TABLE news_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    title           VARCHAR(500) NOT NULL,
    url             VARCHAR(1000) NOT NULL,
    published_at    TIMESTAMPTZ NOT NULL,
    sentiment       NUMERIC(5, 4), -- -1.0000 .. 1.0000
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_news_items_url UNIQUE (url)
);

CREATE INDEX idx_news_items_ticker_published ON news_items (ticker_id, published_at DESC);
