/****** Object:  Table [dbo].[StockQuote]    Script Date: 01.02.2026 09:47:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockQuote] (
  [Id] [int] IDENTITY(1, 1) NOT NULL,
  [Symbol] [varchar](50) NOT NULL,
  [OpenPrice] [decimal](18, 2) NOT NULL,
  [HighPrice] [decimal](18, 2) NOT NULL,
  [LowPrice] [decimal](18, 2) NOT NULL,
  [ClosePrice] [decimal](18, 2) NOT NULL,
  [Volume] [bigint] NOT NULL,
  [MarketCap] [decimal](18, 2) NOT NULL,
  [Timestamp] [datetime] NOT NULL,
  CONSTRAINT [PK_StockQuote] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (
    PAD_INDEX = OFF,
    STATISTICS_NORECOMPUTE = OFF,
    IGNORE_DUP_KEY = OFF,
    ALLOW_ROW_LOCKS = ON,
    ALLOW_PAGE_LOCKS = ON
    )
  ON [PRIMARY]
  ) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[StockQuoteResult]    Script Date: 01.02.2026 09:47:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockQuoteResult] (
  [Id] [int] IDENTITY(1, 1) NOT NULL,
  [StockQuoteId] [int] NOT NULL,
  [StockQuoteScriptId] [int] NOT NULL,
  [Result] [nvarchar](max) NOT NULL,
  CONSTRAINT [PK_StockQuoteResult] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (
    PAD_INDEX = OFF,
    STATISTICS_NORECOMPUTE = OFF,
    IGNORE_DUP_KEY = OFF,
    ALLOW_ROW_LOCKS = ON,
    ALLOW_PAGE_LOCKS = ON
    )
  ON [PRIMARY]
  ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

/****** Object:  Table [dbo].[StockQuoteScript]    Script Date: 01.02.2026 09:47:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[StockQuoteScript] (
  [Id] [int] IDENTITY(1, 1) NOT NULL,
  [Name] [nvarchar](255) NOT NULL,
  [Script] [nvarchar](max) NOT NULL,
  [Binary] [varbinary](max) NOT NULL,
  [ScriptHash] [int] NOT NULL,
  CONSTRAINT [PK_StockQuoteScript] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (
    PAD_INDEX = OFF,
    STATISTICS_NORECOMPUTE = OFF,
    IGNORE_DUP_KEY = OFF,
    ALLOW_ROW_LOCKS = ON,
    ALLOW_PAGE_LOCKS = ON
    )
  ON [PRIMARY]
  ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [IX_UniqueScriptNamePerStockQuote]    Script Date: 01.02.2026 09:47:34 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UniqueScriptNamePerStockQuote] ON [dbo].[StockQuoteScript] (
  [Name] ASC,
  [ScriptHash] ASC
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      IGNORE_DUP_KEY = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

ALTER TABLE [dbo].[StockQuoteResult]
  WITH CHECK ADD CONSTRAINT [FK_StockQuoteResult_StockQuote] FOREIGN KEY ([StockQuoteId]) REFERENCES [dbo].[StockQuote]([Id])
GO

ALTER TABLE [dbo].[StockQuoteResult] CHECK CONSTRAINT [FK_StockQuoteResult_StockQuote]
GO

ALTER TABLE [dbo].[StockQuoteResult]
  WITH CHECK ADD CONSTRAINT [FK_StockQuoteResult_StockQuoteScript] FOREIGN KEY ([StockQuoteScriptId]) REFERENCES [dbo].[StockQuoteScript]([Id])
GO

ALTER TABLE [dbo].[StockQuoteResult] CHECK CONSTRAINT [FK_StockQuoteResult_StockQuoteScript]
GO

ALTER DATABASE [BusinessApi]

SET READ_WRITE
GO


