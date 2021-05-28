
#include <stdio.h>

int main()
{
	int A = 10, B = 5;
	char C, D, E, G, Q, W, R, T;
	int Out = 0, Out1 = 0, Out2 = 0, Out3 = 0, Out4 = 0, OutFinal = 0;
	if (A > B){
		Out1 = C;
	}
	else{
		Out1 = D;
	}
	
	Out2 = (Out1 || C || E);
	
	Out3 = (W && Q && R && T && E);

	Out4 = !Out1;
	
	Out = (!(Out1) && Out2 && Out3);
	
	if (Out1 == 1){
		OutFinal = C || G;
	}
	else {
		OutFinal = C && G;
	}	
}