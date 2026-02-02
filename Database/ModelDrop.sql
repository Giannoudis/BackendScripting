ALTER TABLE [dbo].[StockQuoteResult]

DROP CONSTRAINT [FK_StockQuoteResult_StockQuoteScript]
GO

ALTER TABLE [dbo].[StockQuoteResult]

DROP CONSTRAINT [FK_StockQuoteResult_StockQuote]
GO

/****** Object:  Table [dbo].[StockQuoteScript]    Script Date: 01.02.2026 09:49:30 ******/
DROP TABLE [dbo].[StockQuoteScript]
GO

/****** Object:  Table [dbo].[StockQuoteResult]    Script Date: 01.02.2026 09:49:30 ******/
DROP TABLE [dbo].[StockQuoteResult]
GO

/****** Object:  Table [dbo].[StockQuote]    Script Date: 01.02.2026 09:49:30 ******/
DROP TABLE [dbo].[StockQuote]
GO
