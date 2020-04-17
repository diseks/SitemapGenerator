Simple sitemap generator 
=========

## Overview

Simple sitemap generator helps you to generate sitemap files for any database tables.

Create views for each entity you want to generate sitemap like this:
```
CREATE VIEW [dbo].[SitemapProducts]
AS
     SELECT ProductId AS Id, 
            ISNULL(DateModified, GETDATE()) AS DateModified
     FROM dbo.Products;
GO
```
Then, add this view in configuration file:
```
"FilterSettings": [
    {
      "FileName": "products",
      "Table": "dbo.SitemapProducts",
      "FetchCount": 10000,
      "Route": "/product/{id}"
    },
    {
      "FileName": "vacancies",
      "Table": "dbo.SitemapVacancies",
      "FetchCount": 0,
      "Route": "/vacancy/details/{id}"
    },
    {
      "FileName": "shops",
      "Table": "dbo.SitemapShops",
      "FetchCount": 0,
      "Route": "/shop/{id}"
    }
  ]
```
Set destination folder path:
```
"FileHandlerSettings": {
    "DestinationFolderPath": "D:\\temp\\Sitemap"
  },
```
Run application and check destination folder.