u s i n g   U n i t y E n g i n e ;  
 u s i n g   S y s t e m . C o l l e c t i o n s ;  
 u s i n g   S y s t e m . C o l l e c t i o n s . G e n e r i c ;  
 u s i n g   S y s t e m ;  
  
  
 [ S e r i a l i z a b l e ]  
 p u b l i c   c l a s s   E v e n t     {  
 	 p u b l i c   d o u b l e   T i m e s t a m p ;  
 }  
  
 p u b l i c   c l a s s   C o m p a r e E v e n t   :   I C o m p a r e r < E v e n t >  
 {  
 	 s t a t i c   I C o m p a r e r < E v e n t >   c o m p a r e r   =   n e w   C o m p a r e E v e n t ( ) ;  
  
 	 p u b l i c   i n t   C o m p a r e ( E v e n t   x ,   E v e n t   y )  
 	 {  
 	 	 i f   ( x   = =   y )         r e t u r n   0 ;  
 	 	 i f   ( x   = =   n u l l )   r e t u r n   - 1 ;  
 	 	 i f   ( y   = =   n u l l )   r e t u r n   1 ;  
 	 	 i f   ( x . T i m e s t a m p   >   y . T i m e s t a m p )  
 	 	 	 r e t u r n   1 ;  
 	 	 i f   ( x . T i m e s t a m p   <   y . T i m e s t a m p )  
 	 	 	 r e t u r n   - 1 ;  
  
 	 	 r e t u r n   0 ;  
 	 }  
 }  
  
  
 [ S e r i a l i z a b l e ]    
 p u b l i c   c l a s s   D a t a P o i n t : E v e n t     {  
 	  
 	 p u b l i c   d o u b l e [ ]   V a l u e s ;  
 	 p u b l i c   s t r i n g [ ]   T e x t s ;  
 	 p u b l i c   B a s i c D a t a S e r i e s   o r i g i n   =   n u l l ;  
  
 	 p u b l i c   D a t a P o i n t   C l o n e ( )   {  
 	 	 D a t a P o i n t   c l o n e   =   n e w   D a t a P o i n t ( ) ;  
  
 	 	 i f   ( V a l u e s   ! =   n u l l )  
 	 	 	 c l o n e . V a l u e s   =   ( d o u b l e [ ] ) V a l u e s . C l o n e   ( ) ;  
  
 	 	 i f   ( T e x t s   ! =   n u l l )  
 	 	 	 c l o n e . T e x t s   =   ( s t r i n g [ ] ) T e x t s . C l o n e   ( ) ;  
 	 	  
 	 	 c l o n e . T i m e s t a m p   =   T i m e s t a m p ;  
 	 	 r e t u r n   c l o n e ;  
 	 }  
  
  
  
 	 p u b l i c   D a t a P o i n t   A d d ( D a t a P o i n t   a u g e n d )   {  
 	 	 D a t a P o i n t   c l o n e ;  
 	 	 i n t   f i r s t , s e c o n d , l a r g e s t ;  
 	 	 d o u b l e   f i r s t v a l , s e c o n d v a l ;  
  
 	 	 i f   ( a u g e n d   = =   n u l l )  
 	 	 	 r e t u r n   t h i s ;  
  
 	 	 i f   ( a u g e n d . V a l u e s   = =   n u l l )  
 	 	 	 r e t u r n   t h i s ;  
  
 	 	 i f   ( V a l u e s   = =   n u l l )  
 	 	 	 r e t u r n   a u g e n d ;  
 	 	  
 	 	 f i r s t   =   V a l u e s . L e n g t h ;  
 	 	 s e c o n d   =   a u g e n d . V a l u e s . L e n g t h ;  
  
 	 	 l a r g e s t   =   f i r s t   >   s e c o n d   ?   f i r s t   :   s e c o n d ;  
  
 	 	 c l o n e   =   n e w   D a t a P o i n t ( ) ;  
 	 	 c l o n e . V a l u e s   =   n e w   d o u b l e [ l a r g e s t ] ;  
  
 	 	 f o r   ( i n t   i   =   0 ;   i   <   l a r g e s t ;   i + + )   {  
  
 	 	 	 i f   ( f i r s t   < =   i )  
 	 	 	 	 f i r s t v a l   =   0 . 0 ;  
 	 	 	 e l s e  
 	 	 	 	 f i r s t v a l   =   V a l u e s   [ i ] ;  
  
 	 	 	 i f   ( f i r s t v a l   = =   n u l l )  
 	 	 	 	 f i r s t v a l   =   0 . 0 ;  
 	 	 	  
 	 	 	 i f     ( s e c o n d   < =   i )  
 	 	 	 	 s e c o n d v a l   =   0 . 0 ;  
 	 	 	 e l s e  
 	 	 	 	 s e c o n d v a l   =   a u g e n d . V a l u e s   [ i ] ;  
  
 	 	 	 i f   ( s e c o n d v a l   = =   n u l l )  
 	 	 	 	 s e c o n d v a l   =   0 . 0 ;  
  
 	 	 	 c l o n e . V a l u e s   [ i ]   =   f i r s t v a l   +   s e c o n d v a l ;  
 	 	 }  
  
 	 	 r e t u r n   c l o n e ;  
  
 	 }  
  
 	 p u b l i c   D a t a P o i n t   D i v ( D a t a P o i n t   a u g e n d )   {  
 	 	 D a t a P o i n t   c l o n e ;  
 	 	 i n t   f i r s t , s e c o n d , l a r g e s t ;  
 	 	 d o u b l e   f i r s t v a l , s e c o n d v a l ;  
  
 	 	 i f   ( a u g e n d   = =   n u l l )  
 	 	 	 r e t u r n   n u l l ;  
  
 	 	 i f   ( a u g e n d . V a l u e s   = =   n u l l )  
 	 	 	 r e t u r n   n u l l ;  
  
 	 	 i f   ( V a l u e s   = =   n u l l )  
 	 	 	 r e t u r n   n u l l ;  
  
 	 	 f i r s t   =   V a l u e s . L e n g t h ;  
 	 	 s e c o n d   =   a u g e n d . V a l u e s . L e n g t h ;  
  
 	 	 l a r g e s t   =   f i r s t   >   s e c o n d   ?   f i r s t   :   s e c o n d ;  
  
 	 	 c l o n e   =   n e w   D a t a P o i n t ( ) ;  
 	 	 c l o n e . V a l u e s   =   n e w   d o u b l e [ l a r g e s t ] ;  
  
 	 	 f o r   ( i n t   i   =   0 ;   i   <   l a r g e s t ;   i + + )   {  
  
 	 	 	 i f   ( f i r s t   < =   i )  
 	 	 	 	 f i r s t v a l   =   0 . 0 ;  
 	 	 	 e l s e  
 	 	 	 	 f i r s t v a l   =   V a l u e s   [ i ] ;  
  
 	 	 	 i f   ( f i r s t v a l   = =   n u l l )  
 	 	 	 	 f i r s t v a l   =   0 . 0 ;  
  
 	 	 	 i f     ( s e c o n d   < =   i )  
 	 	 	 	 s e c o n d v a l   =   0 . 0 ;  
 	 	 	 e l s e  
 	 	 	 	 s e c o n d v a l   =   a u g e n d . V a l u e s   [ i ] ;  
  
 	 	 	 i f   ( s e c o n d v a l   = =   n u l l )  
 	 	 	 	 s e c o n d v a l   =   0 . 0 ;  
  
 	 	 	 c l o n e . V a l u e s   [ i ]   =   f i r s t v a l   /   s e c o n d v a l ;  
 	 	 }  
  
 	 	 r e t u r n   c l o n e ;  
 	 }  
  
 }  
  
 [ S e r i a l i z a b l e ]  
 p u b l i c   c l a s s   B a s i c D a t a S e r i e s  
 {  
 	 p u b l i c   L i s t < D a t a P o i n t >   D a t a   =   n e w   L i s t < D a t a P o i n t > ( ) ;  
 	 p u b l i c   i n t   i n d e x   =   0 ;  
  
  
 	 p u b l i c   D a t a P o i n t   G e t C u r r e n t ( )   {  
 	 	 r e t u r n   D a t a   [ i n d e x ] ;  
 	 }  
  
 	 p u b l i c   D a t a P o i n t   G e t P r e v i o u s ( )   {  
 	 	 i f   ( i n d e x   <   1 )  
 	 	 	 r e t u r n   n u l l ;  
  
 	 	 r e t u r n   D a t a   [ i n d e x - 1 ] ;  
 	 }  
  
  
 	 p u b l i c   b o o l   A t E n d ( )   {  
 	 	 r e t u r n   ! ( i n d e x   <   D a t a . C o u n t ) ;  
 	 }  
  
 	 p u b l i c   b o o l   N e x t ( )   {  
 	 	 i n d e x + + ;  
 	 	 r e t u r n   A t E n d   ( ) ;  
 	 }  
  
 	 p u b l i c   D a t a P o i n t   G e t D a t a P o i n t A t ( d o u b l e   T i m e S t a m p )   {  
 	 	 f o r   ( i n t   i   =   1 ;   i   <   D a t a . C o u n t ;   i + + )   {  
 	 	 	 i f   ( D a t a   [ i ] . T i m e s t a m p   >   T i m e S t a m p   & &   D a t a   [ i - 1 ] . T i m e s t a m p   < =   T i m e S t a m p )  
 	 	 	 	 r e t u r n   D a t a   [ i   -   1 ] ;  
 	 	 }  
  
 	 	 r e t u r n   n u l l ;  
 	 }  
  
 	 p u b l i c   d o u b l e   C u r r e n t T i m e ( )   {  
  
 	 	 i f   ( ! ( i n d e x   <   D a t a . C o u n t ) )  
 	 	 	 r e t u r n   D o u b l e . N a N ;  
  
 	 	 r e t u r n   D a t a [ i n d e x ] . T i m e s t a m p ;  
 	 }  
  
 	 p u b l i c   d o u b l e   N e x t T i m e ( )   {  
  
 	 	 i f   ( ! ( i n d e x   <   D a t a . C o u n t   -   1 ) )  
 	 	 	 r e t u r n   D o u b l e . N a N ;  
  
 	 	 r e t u r n   D a t a [ i n d e x + 1 ] . T i m e s t a m p ;  
 	 }  
  
         p u b l i c   D a t a P o i n t   O p e r a t i o n ( i n t   t y p e )  
         {  
                 D a t a P o i n t   R e s   =   n u l l , F i r s t V a l i d   =   n u l l ;  
                 i n t   n V a l u e s   =   0 ;    
  
                 / / G e t   t h e   f i r s t   v a l i d   p o i n t   a s   w e l l   a s   t h e   w i d t h  
                 f o r e a c h   ( D a t a P o i n t   d a t a   i n   D a t a )   {  
                         i f   ( d a t a   = =   n u l l )  
                                 c o n t i n u e ;  
  
                         / / i f   ( F i r s t V a l i d   = =   n u l l )  
                         / /         F i r s t V a l i d   =   d a t a ;  
  
                         i f   ( d a t a . V a l u e s . L e n g t h   >   n V a l u e s )  
                                 n V a l u e s   =   d a t a . V a l u e s . L e n g t h ;  
  
                 }  
  
                 i f   ( n V a l u e s   = =   0 )  
                         r e t u r n   n u l l ;  
  
                 / / S u m   =   F i r s t V a l i d . C l o n e ( ) ;  
                 R e s   =   n e w   D a t a P o i n t ( ) ;  
                 R e s . V a l u e s   =   n e w   d o u b l e [ n V a l u e s ] ;  
  
                  
                 f o r e a c h   ( D a t a P o i n t   d a t a   i n   D a t a )  
                 {  
                         i f   ( d a t a   = =   n u l l )  
                                 c o n t i n u e ;  
                         i f   ( t y p e   = =   0 )  
                                 R e s   =   R e s . A d d ( d a t a ) ;  
                         e l s e   i f   ( t y p e   = =   1 )  
                                 R e s   =   R e s . D i v ( d a t a ) ;  
                 }  
  
                 r e t u r n   R e s ;  
  
         }  
  
         p u b l i c   D a t a P o i n t   S u m ( )  
         {  
                 r e t u r n   O p e r a t i o n ( 0 ) ;  
         }  
  
         p u b l i c   D a t a P o i n t   D i v ( )  
         {  
                 r e t u r n   O p e r a t i o n ( 1 ) ;  
         }  
          
  
  
  
  
         / / 	 p u b l i c   v o i d   M e r g e S t a r c a s e ( D a t a P o i n t s   O t h e r )   {  
         / / 	 	 D a t a P o i n t s   l e a d i n g   =   n u l l ,   n o n l e a d i n g   =   n u l l ,   t m p ;  
         / / 	 	 D a t a P o i n t   p o i n t ;  
         / /  
         / / 	 	 / /   D o   w e   n e e d   t o   a l i g n   t h e   t w o   d a t a s e r i e s ?  
         / / 	 	 i f   (   O t h e r . C u r r e n t T i m e ( )   = =   t h i s . C u r r e n t T i m e ( )   )   {  
         / / 	 	 	 / / N o  
         / / 	 	 	 l e a d i n g   =   n u l l ;  
         / / 	 	 	 n o n l e a d i n g   =   n u l l ;  
         / / 	 	 }   e l s e   {  
         / / 	 	 	 / / S e t   c u r r e n t   t o   t h e   o n e   w i t h   t h e   f i r s t   p o i n t  
         / / 	 	 	 i f   ( O t h e r . C u r r e n t T i m e ( )   <   t h i s . C u r r e n t T i m e ( )   )   {  
         / / 	 	 	 	 l e a d i n g   =   O t h e r ;  
         / / 	 	 	 	 n o n l e a d i n g   =   t h i s ;  
         / / 	 	 	 }   e l s e   {  
         / / 	 	 	 	 l e a d i n g   =   t h i s ;  
         / / 	 	 	 	 n o n l e a d i n g   =   O t h e r ;  
         / / 	 	 	 }  
         / /  
         / /  
         / / 	 	 }  
         / /  
         / / 	 	 / / C o n t i n u e   u n t i l   w e   r u n   o u t   o f   d a t a .    
         / / 	 	 w h i l e   (   ! t h i s . A t E n d ( )   | |   ! O t h e r . A t E n d ( )   )   {  
         / / 	 	 	 / / I f   m i s s a l i n g e d   f i n d   n e x t   p o i n t  
         / / 	 	 	 i f   ( l e a d i n g   ! =   n u l l )   {  
         / / 	 	 	 	 i f   ( l e a d i n g . N e x t T i m e   ( )   >   n o n l e a d i n g . C u r r e n t T i m e   ( ) )   {  
         / / 	 	 	 	 	 t m p   =   l e a d i n g ;  
         / / 	 	 	 	 	 l e a d i n g   =   n o n l e a d i n g ;  
         / / 	 	 	 	 	 n o n l e a d i n g   =   t m p ; 	  
         / / 	 	 	 	 }   e l s e   {  
         / / 	 	 	 	 	 l e a d i n g . N e x t   ( ) ;  
         / /  
         / / 	 	 	 	 }  
         / / 	 	 	 }   e l s e   {  
         / /  
         / / 	 	 	 }  
         / /  
         / / 	 	 }  
         / /  
         / / 	 }  
  
 }  
  
 [ S e r i a l i z a b l e ]  
 p u b l i c   c l a s s   B a s i c D a t a S e r i e s C o l l e c t i o n   {  
 	 p u b l i c   L i s t < B a s i c D a t a S e r i e s >   C o l l e c t i o n   =   n e w   L i s t < B a s i c D a t a S e r i e s > ( ) ;  
  
 	 i n t   i n d e x   =   - 1 ;  
 	 d o u b l e   A t   =   D o u b l e . N e g a t i v e I n f i n i t y ;  
  
  
 	 / / R e t u r n s   t h e   d a t a s e r i e s   w h i c h   w i t h   t h e   p o i n t e r   t o   t h e   e a r l i e s   t i m e s t a m p  
 	 p u b l i c   B a s i c D a t a S e r i e s   G e t N e x t P o i n t ( )   {  
  
 	 	 d o u b l e   e a r l i e s t   =   D o u b l e . P o s i t i v e I n f i n i t y ;  
 	 	 B a s i c D a t a S e r i e s   r e s u l t   =   n u l l ;  
  
 	 	 f o r e a c h   ( B a s i c D a t a S e r i e s   s e r i e   i n   C o l l e c t i o n )   {  
 	 	 	 i f   ( s e r i e . C u r r e n t T i m e ( )   <   e a r l i e s t   & &   s e r i e . C u r r e n t T i m e ( )   >   A t )   {  
 	 	 	 	 e a r l i e s t   =   s e r i e . C u r r e n t T i m e ( ) ;  
 	 	 	 	 r e s u l t   =   s e r i e ;  
 	 	 	 }  
 	 	 	 	  
 	 	 }  
  
 	 	 A t   =   e a r l i e s t ;  
  
 	 	 r e t u r n   r e s u l t ;  
 	 }  
  
 	 p u b l i c   B a s i c D a t a S e r i e s   G e t N e x t P o i n t s ( )   {  
 	 	  
 	 	 B a s i c D a t a S e r i e s   H a s N e x t P o i n t ;  
 	 	 d o u b l e   T i m e S t a m p ;  
  
 	 	 / / G e t   t h e   n e x t   p o i n t .  
 	 	 H a s N e x t P o i n t   =   G e t N e x t P o i n t   ( ) ;  
  
 	 	 i f   ( H a s N e x t P o i n t   = =   n u l l )  
 	 	 	 r e t u r n   n u l l ;  
  
 	 	 / / S a v e   t h e   t i m e s t a m p .  
 	 	 T i m e S t a m p   =   H a s N e x t P o i n t . C u r r e n t T i m e   ( ) ;  
 	 	 / / M o v e   t h a t   s e r i e s   f o r w a r d .  
 	 	 H a s N e x t P o i n t . N e x t   ( ) ;  
  
 	 	 D a t a P o i n t   d p ;  
  
 	 	 B a s i c D a t a S e r i e s   r e s u l t   =   n e w   B a s i c D a t a S e r i e s   ( ) ;  
  
 	 	 f o r e a c h   ( B a s i c D a t a S e r i e s   s e r i e   i n   C o l l e c t i o n )   {  
 	 	 	 d p   =   s e r i e . G e t D a t a P o i n t A t   ( T i m e S t a m p ) ;  
 	 	 	 r e s u l t . D a t a . A d d   ( d p ) ;  
 	 	 }  
  
 	 	 r e t u r n   r e s u l t ;  
 	 }  
  
  
 	 p u b l i c   b o o l   A l l A t E n d ( )   {  
 	 	 f o r e a c h   ( B a s i c D a t a S e r i e s   s e r i e   i n   C o l l e c t i o n )   {  
 	 	 	 i f   ( ! s e r i e . A t E n d   ( ) )  
 	 	 	 	 r e t u r n   f a l s e ;  
 	 	 }  
  
 	 	 r e t u r n   t r u e ;  
 	 }  
  
 	 / / S t a i r c a s e   a s s u m e s   t h a t   a   v a l u e   i s   v a l i d   u n t i l   w e   g e t   a   n e w   v a l u e .    
 	 / / I n   t h i s   c a s e   m i s s a l i g h e d   d a t a   n o t   i n s i d e   t h e   r a n g e   o f   t h e   o t h e r   s e r i e s   w i l l   b e   d i s c a r e d .    
 	 p u b l i c   B a s i c D a t a S e r i e s   G e t S t a i r c a s e S u m O f S e r i e s ( )   {  
 	 	 B a s i c D a t a S e r i e s   p o i n t s ;  
 	 	 B a s i c D a t a S e r i e s   r e s u l t   =   n e w   B a s i c D a t a S e r i e s   ( ) ;  
  
 	 	 p o i n t s   =   G e t N e x t P o i n t s   ( ) ;  
  
 	 	 w h i l e   ( p o i n t s   ! =   n u l l )   {  
  
  
 	 	 	 D a t a P o i n t   S u m   =   n e w   D a t a P o i n t   ( ) ;  
  
 	 	 	 f o r e a c h   ( D a t a P o i n t   p o i n t   i n   p o i n t s . D a t a )   {  
  
 	 	 	 	 / / i f   ( p o i n t   = =   n u l l )   {  
 	 	 	 	 / / 	 S u m   =   n u l l ;  
 	 	 	 	 / / 	 b r e a k ;  
 	 	 	 	 / / }  
  
 	 	 	 	 S u m   =   S u m . A d d   ( p o i n t ) ;  
  
 	 	 	 }  
  
 	 	 	 i f   ( S u m   ! =   n u l l   & &   S u m . V a l u e s   ! =   n u l l )   {  
  
 	 	 	 	 S u m . T i m e s t a m p   =   A t ;  
 	 	 	 	 r e s u l t . D a t a . A d d   ( S u m ) ;  
 	 	 	 }  
  
 	 	 	 p o i n t s   =   G e t N e x t P o i n t s   ( ) ;  
 	 	 }  
  
 	 	 r e t u r n   r e s u l t ;  
 	 }  
  
  
  
 	 p u b l i c   B a s i c D a t a S e r i e s   G e t S t a i r c a s e D i v O f S e r i e s ( )   {  
 	 	 B a s i c D a t a S e r i e s   p o i n t s ;  
 	 	 B a s i c D a t a S e r i e s   r e s u l t   =   n e w   B a s i c D a t a S e r i e s   ( ) ;  
                 i n t   i ;  
  
                 p o i n t s   =   G e t N e x t P o i n t s ( ) ;  
  
                 w h i l e   ( p o i n t s   ! =   n u l l )   {  
  
                         i f   ( p o i n t s . D a t a [ 0 ]   = =   n u l l )  
                         {  
                                 p o i n t s   =   G e t N e x t P o i n t s ( ) ;  
                                 c o n t i n u e ;  
                         }  
  
                         D a t a P o i n t   S u m   =   p o i n t s . D a t a [ 0 ] . C l o n e ( ) ;  
                          
  
 	 	 	 f o r   ( i = 1 ; i < p o i n t s . D a t a . C o u n t ; i + + )   {  
  
  
 	 	 	 	 S u m   =   S u m . D i v   ( p o i n t s . D a t a [ i ] ) ;  
  
 	 	 	 }  
  
 	 	 	 i f   ( S u m   ! =   n u l l   & &   S u m . V a l u e s   ! =   n u l l )   {  
  
 	 	 	 	 S u m . T i m e s t a m p   =   A t ;  
 	 	 	 	 r e s u l t . D a t a . A d d   ( S u m ) ;  
 	 	 	 }  
  
 	 	 	 p o i n t s   =   G e t N e x t P o i n t s   ( ) ;  
 	 	 }  
  
 	 	 r e t u r n   r e s u l t ;  
 	 }  
  
  
 } 	  
  
 
