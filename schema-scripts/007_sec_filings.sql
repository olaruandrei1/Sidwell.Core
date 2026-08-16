CREATE TABLE sec_filings (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ticker_id       UUID NOT NULL REFERENCES tickers(id) ON DELETE CASCADE,
    form_type       VARCHAR(20) NOT NULL,
    filing_date     DATE NOT NULL,
    accession_no    VARCHAR(30) NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT uq_sec_filings_accession_no UNIQUE (accession_no)
);

CREATE INDEX idx_sec_filings_ticker_filing_date ON sec_filings (ticker_id, filing_date DESC);
