# ModernRobot
Modern Trading System

This repository contains files for Modern Trading System.

The idea is to build a stock data database that actualizes prices every day and uses them for data analysis in testing and real trading (instead of downloading every time you ask for prices).
This project contains a multithread WCF server connected to MS SQL database and a client part.

The global goal is to get a successful real-time trading system working on machine learning algorithms (logistic regression and neural networks). 

I WILL NOT commit my trading strategies (obviously) - a few files in Strategies folder, but I left one simple algorithm (gives no profit). 

Contents of WCF server part:
1) Database actualizer (saves data to MS SQL database every day for selected financial instruments). Stable.
2) FINAM downloader. Classes for downloading stock data from FINAM servers. Stable.
3) Calculator. Contains trading algorithms for testing and real trading. Stable, sensitive data removed.
4) Simple unit tests for trading algorithms.

Contents of client part:
1) DBEditor. A simple application that allows to add a new instrument to DB 
for next actualization (see instrument settings on finam.ru or in link description downloader module). Stable.
2) Silverlight testing module. Client application to get data and run tests on server. Stable. NO LONGER USED. 
Will be rewritten on ASP.NET.

Plans for future:
1) New client part on ASP.NET and minor bug fixes.
2) APPLY machine learning algorithms on sock data to develop new trading strategies.
3) Real market trading driver (Alfa-Direct integration planned)
4) Real trading realization on server.

Please, use this code for your needs on your own risk.
